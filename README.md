# Authentication
Here are the two packages that are primarily used in authentication when using JWT

1. Authentication JWT Bearer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0

**Why**: 
- This is going to help work with Bearer token in jwt
- Helps in setting bearer token options

2. Identity Model Tokens
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.10.0

**Why**: This is the core of jwt to help us in the following
- Creation of tokens
- Validation of tokens
- Rules to the token eg. expiry date