SELECT * FROM TaskEntry WHERE Id = @Id;
SELECT t.*, tt.Id as GlueId FROM Tags t INNER JOIN TasksTags tt on tt.TaskEntryId = tt.TagId WHERE tt.TaskEntryId = @Id;
SELECT * FROM TaskEntryLinks WHERE TaskEntryId = @Id;