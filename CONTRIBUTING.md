# ECommerceNetApp - Contributing Guide
# Code Quality: Running SonarQube Analysis Locally

To help maintain high code quality, please analyze your changes with SonarQube before submitting a pull request.

## Prerequisites

- Docker installed (for running SonarQube server)
- .NET 8.0 SDK
- SonarScanner for .NET

## Steps to Run SonarQube Locally

1. **Start SonarQube Server**: Use Docker to run the SonarQube server.
   ```bash
   docker run -d --name sonarqube -p 9000:9000 sonarqube:lts-community
   ```

2. **Access SonarQube Dashboard**: Open your browser and navigate to [http://localhost:9000](http://localhost:9000). The default credentials are:
   - Username: `admin`
   - Password: `admin`

3. **Install SonarScanner for .NET**: If you haven't already, install the SonarScanner tool.
   ```bash
   dotnet tool install --global dotnet-sonarscanner
   ```
   
4. **Run the Analysis**: Navigate to your project directory and run the following commands:
   ```bash
   dotnet sonarscanner begin /k:"YourProjectKey" /n:"YourProjectName" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="admin"
   dotnet build
   dotnet sonarscanner end /d:sonar.login="admin"
   ```

   Replace `YourProjectKey` and `YourProjectName` with appropriate values for your project.

5. **Review Results**: After the analysis completes, you can view the results in the SonarQube dashboard.

## Add pre-push hook

To ensure that code style violations are caught before pushing changes, you can add a pre-push hook to your Git repository. This will run `dotnet format` to verify code style compliance.

```
#!/bin/sh
dotnet format --verify-no-changes
if [ $? -ne 0 ]; then
  echo "Code style violations found. Please run 'dotnet format' and fix them before pushing."
  exit 1
fi
```
  

## Additional Notes

- Ensure that your code adheres to the quality gates defined in SonarQube.
- Address any issues reported by SonarQube before submitting your pull request.
- Focus on fixing issues with **Major** severity or higher to maintain code quality standards.
- If you encounter any issues, refer to the [SonarQube documentation](https://docs.sonarqube.org/latest/) for troubleshooting tips.
- After fixing issues, re-run the analysis to ensure all changes meet the quality standards.
- Commit your changes and push them to your feature branch before submitting a pull request.
- Make sure to include a summary of the changes and any relevant information in your pull request description.
- Follow the standard GitHub workflow for contributing:
  1. Fork the repository.
  2. Create a feature branch.
  3. Commit your changes.
  4. Submit a pull request.

