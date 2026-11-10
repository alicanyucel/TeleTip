// @ts-nocheck
const hre = require("hardhat");

async function main() {
  const registry = await hre.ethers.deployContract("HealthRecordRegistry");
  await registry.waitForDeployment();

  console.log(`HealthRecordRegistry deployed to: ${await registry.getAddress()}`);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
