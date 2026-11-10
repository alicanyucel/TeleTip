# Local EVM notarization

The application stores only SHA-256 hashes of health records on-chain. Sensitive health data is never sent to the blockchain.

## Start a local Hardhat node

```powershell
cd blockchain
npm install
npm run node
```

In another terminal, deploy the registry:

```powershell
cd blockchain
npm run deploy:local
```

Copy the deployment address into `Blockchain:ContractAddress` using user secrets or an environment variable. Use one of the private keys printed by the local node:

```powershell
dotnet user-secrets init --project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj
dotnet user-secrets set "Blockchain:ContractAddress" "0x..." --project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj
dotnet user-secrets set "Blockchain:PrivateKey" "0x..." --project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj
```

The `IHealthRecordNotarizer` service accepts a record id, record type, and canonical payload. It hashes the payload with SHA-256 and submits the hash to `HealthRecordRegistry`.
