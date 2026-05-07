# Copilot Instructions

## Project Guidelines
- User prefers replacing enums with persisted context entities when values are domain data, and wants default values seeded in the database.
- User prefers seeding context entities through SeedExtensionMethods, following the ForecastRecurrenceRuleType pattern, instead of seeding them in EF model configuration or migrations when avoidable.
- User prefers reusing existing helper methods such as `ConfigureAuditRelations` for EF model configuration where possible, instead of duplicating audit configuration inline.
- User prefers moving business logic out of API services into `MoneyTracker.BusinessLogic` following Vertical Slice Architecture (VSA) patterns.
- User prefers using the 'Dto' suffix when a class is a Data Transfer Object for readability.