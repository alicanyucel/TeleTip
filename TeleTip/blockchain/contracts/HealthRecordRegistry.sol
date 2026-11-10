// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

contract HealthRecordRegistry {
	struct Registration {
		address submitter;
		uint64 registeredAt;
		uint256 blockNumber;
	}

	mapping(bytes32 => Registration) private registrations;

	event HealthRecordRegistered(
		bytes32 indexed recordHash,
		bytes32 indexed recordId,
		address indexed submitter,
		uint256 blockNumber,
		uint64 registeredAt
	);

	function registerHealthRecord(bytes32 recordHash, bytes32 recordId)
		external
		returns (bool alreadyRegistered)
	{
		Registration storage existing = registrations[recordHash];
		if (existing.registeredAt != 0) {
			return true;
		}

		registrations[recordHash] = Registration({
			submitter: msg.sender,
			registeredAt: uint64(block.timestamp),
			blockNumber: block.number
		});

		emit HealthRecordRegistered(
			recordHash,
			recordId,
			msg.sender,
			block.number,
			uint64(block.timestamp)
		);

		return false;
	}

	function getRegistration(bytes32 recordHash)
		external
		view
		returns (address submitter, uint64 registeredAt, uint256 blockNumber)
	{
		Registration memory registration = registrations[recordHash];
		require(registration.registeredAt != 0, "Record is not registered");
		return (
			registration.submitter,
			registration.registeredAt,
			registration.blockNumber
		);
	}

	function isRegistered(bytes32 recordHash) external view returns (bool) {
		return registrations[recordHash].registeredAt != 0;
	}
}
