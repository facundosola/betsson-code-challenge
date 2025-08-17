# QA Backend Code Challenge

This repository contains the code challenge for QA Backend Engineer candidates. The main goal is to design, implement, and validate automated tests for a digital wallet API.

## Project Structure

```
src/
	Betsson.OnlineWallets/           # Business logic
	Betsson.OnlineWallets.Data/      # Data access and repositories
	Betsson.OnlineWallets.Web/       # Web API (ASP.NET Core)
test/
	Betsson.OnlineWallets.UnitTests/ # Unit tests for services
	Betsson.OnlineWallets.ApiTests/  # API/integration tests
```

## Testing Frameworks Used

- **xUnit**: Main test runner for .NET.
- **Moq**: Library for creating mocks and stubs in unit and API tests.
- **AutoMapper** and **Microsoft.AspNetCore.Mvc**: Used in API tests to simulate real controller behavior.

## Automated Tests

The repository includes two layers of automated tests:

- **Unit Tests** (`test/Betsson.OnlineWallets.UnitTests/`):
  Validate the business logic of wallet services in isolation, using mocks for external dependencies.

- **API Tests** (`test/Betsson.OnlineWallets.ApiTests/`):
  Verify the behavior of the exposed API, simulating HTTP requests and validating responses.

### Run the tests

From the root of the project, run:

```
dotnet test
```

## Run the API with Docker

### Build Docker image

Run this command from the directory where the solution file is located:

```
docker build -f src/Betsson.OnlineWallets.Web/Dockerfile .
```

### Run Docker container

```
docker run -p <port>:8080 <image id>
```

### Open Swagger

```
http://localhost:<port>/swagger/index.html
```

## Possible Improvements

- Extract common mock setups and repeated code into helper classes or methods to improve maintainability and reduce duplication.
- Add more edge case scenarios and negative tests for the API layer.
- Increase automation for e2e tests (e.g., using Newman or integration with CI/CD).

Due to time constraints, some of these improvements were not implemented, but they are identified for future work.

---

**Note:**  
Special attention was given to the quality, coverage, and organization of the automated tests.
