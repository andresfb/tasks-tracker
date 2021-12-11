SELECT * FROM TaskEntry WHERE Id = @Id;
SELECT t.*, tt.Id as GlueId FROM Tags t INNER JOIN TasksTags tt on tt.TagId = t.Id WHERE tt.TaskEntryId = @Id;
SELECT * FROM TaskEntryLinks WHERE TaskEntryId = @Id;