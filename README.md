# repro-marten-multiple-streams-same-event

## Usage

```bash
cd MultipleStreamsWithSameEvent.Tests/test-database
docker compose up -d
dotnet test
```

## Issue
Database contains two rows in `mt_doc_team` and `mt_doc_employee` but should contain only one.