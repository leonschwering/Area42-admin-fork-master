# AWS Deployment Guide for Area42

## Prerequisites

- AWS Account with appropriate IAM permissions
- Docker installed locally
- AWS CLI configured
- ECR (Elastic Container Registry) repository created

## Architecture Overview

Area42 uses the following AWS services:

- **ECS (Fargate)** - Container orchestration
- **RDS SQL Server** - Managed database
- **ALB (Application Load Balancer)** - Load balancing
- **VPC** - Network isolation
- **CloudWatch** - Logging and monitoring
- **Secrets Manager** - Secure credential storage

## Step 1: Build and Push Docker Images to ECR

```bash
# Get AWS account ID and login to ECR
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
AWS_REGION="eu-west-1"  # Change to your region

# Login to ECR
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com

# Build API service image
docker build -f Dockerfile.ApiService -t area42-api:latest .
docker tag area42-api:latest $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/area42-api:latest
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/area42-api:latest

# Build Web service image
docker build -f Dockerfile.Web -t area42-web:latest .
docker tag area42-web:latest $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/area42-web:latest
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/area42-web:latest
```

## Step 2: Create AWS Secrets

Store sensitive configuration in AWS Secrets Manager:

```bash
# Database connection string
aws secretsmanager create-secret \
  --name area42/db-connection-string \
  --secret-string "Server=area42-rds.xxxxx.eu-west-1.rds.amazonaws.com;Database=Area42;User Id=admin;Password=YourSecurePassword123!;Encrypt=true;TrustServerCertificate=false;"

# JWT Secret Key
aws secretsmanager create-secret \
  --name area42/jwt-key \
  --secret-string "your-long-random-jwt-key-here-minimum-32-characters"
```

## Step 3: Deploy CloudFormation Stack

```bash
aws cloudformation create-stack \
  --stack-name area42-prod \
  --template-body file://aws/cloudformation-template.json \
  --parameters ParameterKey=Environment,ParameterValue=production \
  --capabilities CAPABILITY_NAMED_IAM \
  --region $AWS_REGION
```

## Step 4: Create ECS Task Definitions

```bash
# Update the task definitions with your ECR image URIs first
# Replace ACCOUNT_ID, REGION, and DOCKER_IMAGE_URI in the JSON files

# Register API task definition
aws ecs register-task-definition \
  --cli-input-json file://aws/ecs-task-definition-api.json

# Register Web task definition
aws ecs register-task-definition \
  --cli-input-json file://aws/ecs-task-definition-web.json
```

## Step 5: Create ECS Services

```bash
# Get the target group ARN and subnet IDs from CloudFormation outputs
CLUSTER_NAME="Area42-Cluster"
TARGET_GROUP_ARN=$(aws cloudformation describe-stacks --stack-name area42-prod --query 'Stacks[0].Outputs[?OutputKey==`TargetGroupArn`].OutputValue' --output text)

# Create API service
aws ecs create-service \
  --cluster $CLUSTER_NAME \
  --service-name area42-api-service \
  --task-definition Area42-API-Task:1 \
  --desired-count 2 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-xxxxx,subnet-yyyyy],securityGroups=[sg-xxxxx],assignPublicIp=DISABLED}" \
  --load-balancers targetGroupArn=$TARGET_GROUP_ARN,containerName=area42-api,containerPort=80 \
  --region $AWS_REGION

# Create Web service
aws ecs create-service \
  --cluster $CLUSTER_NAME \
  --service-name area42-web-service \
  --task-definition Area42-Web-Task:1 \
  --desired-count 2 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-xxxxx,subnet-yyyyy],securityGroups=[sg-xxxxx],assignPublicIp=DISABLED}" \
  --load-balancers targetGroupArn=$TARGET_GROUP_ARN,containerName=area42-web,containerPort=80 \
  --region $AWS_REGION
```

## Step 6: Monitor Deployment

```bash
# Check service status
aws ecs describe-services \
  --cluster $CLUSTER_NAME \
  --services area42-api-service area42-web-service \
  --region $AWS_REGION

# View logs
aws logs tail /ecs/area42-api --follow
aws logs tail /ecs/area42-web --follow
```

## Local Testing with Docker Compose

Before deploying to AWS, test locally with Docker Compose:

```bash
# Start all services (API, Web, SQL Server)
docker-compose up

# Access the application
# Web: http://localhost:3000
# API: http://localhost:7001

# Stop services
docker-compose down
```

## Environment Variables

### API Service
- `ASPNETCORE_ENVIRONMENT`: Production
- `ConnectionStrings__DefaultConnection`: From Secrets Manager
- `Jwt__Key`: From Secrets Manager
- `Jwt__Issuer`: Area42
- `Jwt__Audience`: Area42Client

### Web Service
- `ASPNETCORE_ENVIRONMENT`: Production
- `ApiUrl`: http://area42-api-service:80

## Scaling and Auto-Scaling

To enable auto-scaling:

```bash
# Register scalable target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --resource-id service/Area42-Cluster/area42-api-service \
  --scalable-dimension ecs:service:DesiredCount \
  --min-capacity 2 \
  --max-capacity 10

# Create scaling policy (CPU-based)
aws application-autoscaling put-scaling-policy \
  --policy-name area42-api-cpu-scaling \
  --service-namespace ecs \
  --resource-id service/Area42-Cluster/area42-api-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration file://scaling-policy.json
```

## Continuous Integration/Deployment (CI/CD)

See `.github/workflows/aws-deploy.yml` for automated deployment pipeline.

## Database Migrations

The application automatically runs migrations on startup. If you need to manually migrate:

```bash
# SSH into the container
aws ecs execute-command \
  --cluster Area42-Cluster \
  --task <TASK_ID> \
  --container area42-api \
  --command "/bin/bash" \
  --interactive \
  --region $AWS_REGION

# Inside container
dotnet ef database update --context Area42Context
```

## Security Best Practices

✅ Use AWS Secrets Manager for all secrets  
✅ Enable encryption in transit (HTTPS/TLS)  
✅ Use IAM roles for ECS tasks  
✅ Enable VPC endpoints for private communication  
✅ Use Security Groups to restrict traffic  
✅ Enable CloudTrail for audit logging  
✅ Regularly update container images  

## Troubleshooting

### Service won't start
- Check CloudWatch logs: `aws logs tail /ecs/area42-api`
- Verify Secrets Manager permissions
- Check security group rules

### Database connection errors
- Verify RDS endpoint is accessible
- Check security group rules between ECS and RDS
- Verify connection string in Secrets Manager

### API communication issues
- Check service discovery DNS names
- Verify security groups allow communication
- Check ALB target health

## Rollback

To rollback to a previous version:

```bash
# Update service with previous task definition version
aws ecs update-service \
  --cluster Area42-Cluster \
  --service area42-api-service \
  --task-definition Area42-API-Task:X \
  --region $AWS_REGION

# Wait for service to stabilize
aws ecs wait services-stable \
  --cluster Area42-Cluster \
  --services area42-api-service
```

## Cost Optimization

- Use Fargate Spot for non-critical environments
- Set appropriate CPU/memory allocations
- Use auto-scaling policies
- Enable RDS backups only for production

## Support and Monitoring

- Enable CloudWatch Container Insights
- Set up CloudWatch alarms for critical metrics
- Use X-Ray for distributed tracing
- Monitor costs with AWS Budgets
