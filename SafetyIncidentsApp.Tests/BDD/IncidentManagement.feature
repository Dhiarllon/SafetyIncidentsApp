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
        | Location     | Andar 2 - Ala Norte     |
        | Type         | Fall                     |
        | Description  | Escorregou no piso molhado |
        | Severity     | Low                      |
        | Cost         | 500                      |
    Then the incident should be created successfully
    And the incident status should be "Reported"
    And the incident should not require manager approval
    And the incident should not require safety review

Scenario: Create a high severity incident requiring approval
    Given I want to report a high severity incident
    When I create an incident with the following details:
        | Field        | Value                    |
        | Location     | Andar 10 - Ala Sul      |
        | Type         | Fall                     |
        | Description  | Queda de altura crítica  |
        | Severity     | High                     |
        | Cost         | 15000                    |
    Then the incident should be created successfully
    And the incident status should be "PendingApproval"
    And the incident should require manager approval
    And the incident should require safety review

Scenario: Create an electric shock incident requiring investigation notes
    Given I want to report an electric shock incident
    When I create an incident with the following details:
        | Field        | Value                    |
        | Location     | Subestação              |
        | Type         | ElectricShock            |
        | Description  | Choque elétrico          |
        | Severity     | Medium                   |
        | Cost         | 8000                     |
        | InvestigationNotes | Falta de isolamento |
    Then the incident should be created successfully
    And the incident should require safety review

Scenario: Create an incident with non-existent employee
    Given I want to report an incident
    When I create an incident with a non-existent employee
    Then the system should return an error
    And the error message should contain "Reported by employee not found"

Scenario: Approve a pending incident
    Given there is a pending incident requiring approval
    When I approve the incident as "Manager Name"
    Then the incident status should be "Approved"
    And the manager approval date should be set
    And the manager approved by should be "Manager Name"

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

Scenario: Get high risk incidents
    Given there are incidents with different risk levels
    When I request high risk incidents
    Then I should receive incidents with high severity or cost over 10000
    And all returned incidents should be high risk

Scenario: Create incident with PPE violation and untrained employee
    Given I want to report a PPE violation incident
    And the involved employee has not had safety training in the last 6 months
    When I create the incident with the untrained employee
    Then the system should return an error
    And the error message should contain "must have recent safety training" 