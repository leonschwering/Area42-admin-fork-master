# Area42 Deployment Checklist

## Pre-Deployment

### Code Quality
- [ ] All code committed to Git
- [ ] Code review completed
- [ ] Unit tests passing
- [ ] Build succeeding with no warnings
- [ ] Security scan completed (SonarQube/similar)
- [ ] Dependencies updated to latest secure versions

### Configuration
- [ ] `appsettings.Production.json` configured
- [ ] All secrets moved to AWS Secrets Manager
- [ ] API URL configured correctly
- [ ] CORS settings reviewed
- [ ] Logging configured for production
- [ ] Error handling tested

### Database
- [ ] Connection string verified
- [ ] Database backups configured
- [ ] Migrations tested locally
- [ ] Seed data verified
- [ ] RDS encryption enabled

## Local Testing

### Functionality
- [ ] Admin login works
- [ ] All admin dashboard sections accessible
- [ ] Accommodations page loads correctly
- [ ] Attractions page displays with links
- [ ] Company contact info visible
- [ ] Language switching works (Dutch/English)
- [ ] Images load properly
- [ ] Forms submit correctly

### API Testing
- [ ] API endpoints responding
- [ ] Authentication working
- [ ] CORS not blocked
- [ ] Error responses formatted correctly
- [ ] Rate limiting tested (if implemented)

### Admin Features
- [ ] Super Admin can access all sections
- [ ] Role-based permissions enforced
- [ ] Kill switch functional
- [ ] Audit logs recording
- [ ] Security dashboard showing data

## AWS Preparation

### Account Setup
- [ ] AWS Account created
- [ ] IAM roles configured
- [ ] ECR repositories created (area42-api, area42-web)
- [ ] Secrets Manager prepared
- [ ] CloudWatch log groups created

### Infrastructure
- [ ] VPC configured
- [ ] Subnets created (public and private)
- [ ] Security groups defined
- [ ] RDS SQL Server instance provisioned
- [ ] Application Load Balancer configured

### Secrets
- [ ] Database connection string in Secrets Manager
- [ ] JWT key in Secrets Manager
- [ ] API key in Secrets Manager (if needed)
- [ ] HTTPS certificates prepared

## Docker Build & Push

```bash
# Replace with your values
AWS_ACCOUNT_ID=123456789
AWS_REGION=eu-west-1

# Login to ECR
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com

# Build and push API
docker build -f Dockerfile.ApiService -t $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/area42-api:latest .
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/area42-api:latest

# Build and push Web
docker build -f Dockerfile.Web -t $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/area42-web:latest .
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/area42-web:latest
```

## AWS Deployment

### Step 1: CloudFormation Stack
- [ ] Stack created successfully
- [ ] VPC resources created
- [ ] RDS instance running
- [ ] Load balancer active
- [ ] Outputs captured

### Step 2: ECS Services
- [ ] Task definitions registered
- [ ] ECS cluster created
- [ ] Services created
- [ ] Task definitions updated with image URIs
- [ ] Desired task count set to minimum 2

### Step 3: Health Checks
- [ ] ECS tasks showing "RUNNING" status
- [ ] ALB target health showing "healthy"
- [ ] CloudWatch logs showing startup messages
- [ ] API responding on endpoint
- [ ] Web app accessible on load balancer DNS

## Post-Deployment

### Smoke Testing
- [ ] Application accessible via ALB DNS
- [ ] Admin login works with test credentials
- [ ] Admin dashboard functional
- [ ] Database migrations completed
- [ ] Seed data loaded
- [ ] API endpoints responding
- [ ] Error handling working

### Monitoring
- [ ] CloudWatch Insights dashboard created
- [ ] CPU/Memory alerts configured
- [ ] Error rate alerts configured
- [ ] Database connection pool monitored
- [ ] Log retention set appropriately

### Security
- [ ] HTTPS enforced (redirect HTTP → HTTPS)
- [ ] Security headers set
- [ ] CORS configured correctly
- [ ] WAF rules applied (if using)
- [ ] DDoS protection enabled
- [ ] Security group rules minimal

### Performance
- [ ] Page load times acceptable (< 2s)
- [ ] API response times acceptable (< 200ms)
- [ ] Database query performance verified
- [ ] Cache configured (if applicable)
- [ ] CDN configured for static assets

### Backup & Recovery
- [ ] RDS automated backups enabled
- [ ] Backup retention verified
- [ ] Test restore procedure
- [ ] Disaster recovery plan documented

## Scaling Configuration

### Auto-Scaling
- [ ] Scalable targets registered
- [ ] CPU-based scaling policy configured (70% threshold)
- [ ] Memory-based scaling policy configured (80% threshold)
- [ ] Min/max capacity set (2-10 instances)
- [ ] Scale-in cooldown configured

## DNS & CDN

### Domain Setup
- [ ] Domain purchased
- [ ] DNS records created pointing to ALB
- [ ] SSL certificate obtained (ACM)
- [ ] HTTPS configured
- [ ] TTL set appropriately

### CDN (Optional)
- [ ] CloudFront distribution created
- [ ] Cache behaviors configured
- [ ] Compression enabled
- [ ] Origin configured (ALB)

## Documentation

### Deployment Docs
- [ ] Deployment guide completed
- [ ] Runbook created for common issues
- [ ] Escalation procedures documented
- [ ] Team trained on deployment

### Application Docs
- [ ] API documentation updated
- [ ] Admin portal guide created
- [ ] User guide for customers
- [ ] Architecture diagram documented

## Rollback Plan

### Preparation
- [ ] Previous stable task definition documented
- [ ] Rollback procedure tested
- [ ] Communication template prepared

### Execution Steps
1. [ ] Stop new tasks
2. [ ] Update ECS service with previous task definition
3. [ ] Wait for service stabilization
4. [ ] Verify application functionality
5. [ ] Send notifications to stakeholders

## Post-Launch Monitoring (First 24 Hours)

### Critical Metrics
- [ ] Error rate < 0.1%
- [ ] API latency < 200ms (p95)
- [ ] CPU usage < 70%
- [ ] Memory usage < 80%
- [ ] Database connections healthy

### User Feedback
- [ ] No critical bug reports
- [ ] Performance complaints addressed
- [ ] Feature functionality verified

## Go-Live Approval

- [ ] All checklist items completed
- [ ] No critical issues
- [ ] Stakeholder sign-off
- [ ] Support team briefed
- [ ] Monitoring active

## Launch Communication

Send notifications to:
- [ ] Development team
- [ ] Operations team
- [ ] Support team
- [ ] Stakeholders
- [ ] Customers (if public-facing)

**Deployment Status**: ✅ Ready for Production

---

## After-Launch (First Week)

- [ ] Daily monitoring of metrics
- [ ] User feedback collection
- [ ] Bug fixes deployed
- [ ] Performance optimization if needed
- [ ] Incident response testing
- [ ] Team retro held

## Notes

- **Deployment Date**: [DATE]
- **Deployed By**: [NAME]
- **Approval From**: [NAME]
- **Critical Contacts**: [CONTACTS]
- **Rollback Contact**: [NAME/NUMBER]
