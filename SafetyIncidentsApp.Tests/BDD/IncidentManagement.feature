Feature: Incident Management
    As a safety manager
    I want to manage safety incidents
    So that I can ensure workplace safety and compliance

Background:
    Given the safety incidents system is running
    And there are employees in the system

Scenario: Create a low severity incident
    Given I want to report a low severity incident
    When I create an incident with the following details:
        | Field        | Value                    |
        | Location     | 2nd Floor - North Wing   |
        | Type         | Fall                     |
        | Description  | Slipped on wet floor     |
        | Severity     | Low                      |
        | Cost         | 500                      |
    Then the incident should be created successfully
    And the incident status should be "Reported"
    And the incident should not require manager approval

Scenario: Create a high severity incident requiring approval
    Given I want to report a high severity incident
    When I create an incident with the following details:
        | Field        | Value                    |
        | Location     | 10th Floor - South Wing  |
        | Type         | Fall                     |
        | Description  | Critical height fall     |
        | Severity     | High                     |
        | Cost         | 15000                    |
    Then the incident should be created successfully
    And the incident status should be "PendingApproval"
    And the incident should require manager approval

Scenario: Create an incident with non-existent employee
    Given I want to report an incident
    When I create an incident with a non-existent employee
    Then the system should return an error
    And the error message should contain "Reported by employee not found"

Scenario: Approve a pending incident
    Given there is a pending incident requiring approval
    When I approve the incident as "Manager Name"
    Then the incident status should be "Approved"

Scenario: Close an incident without approval
    Given there is a pending incident requiring approval
    When I try to close the incident
    Then the system should return an error
    And the error message should contain "requires manager approval"

Scenario: Update an incident
    Given there is an existing incident
    When I update the incident with new details:
        | Field        | Value                    |
        | Location     | Updated location         |
        | Description  | Updated description      |
        | Cost         | 3000                     |
    Then the incident should be updated successfully
    And the incident location should be "Updated location"
    And the incident description should be "Updated description"
    And the incident cost should be 3000

Scenario: Get incidents by severity
    Given there are incidents with different severities
    When I request incidents with severity "High"
    Then I should receive only high severity incidents
    And all returned incidents should have severity "High" 