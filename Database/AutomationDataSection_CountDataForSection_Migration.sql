-- AutomationDataSection_CountDataForSection_Migration.sql
-- Idempotent (safe to re-run). Adds a small helper proc used only by the new delete
-- guard for Sections (see AGENTS.md - "Flow & Section management") - confirmed via
-- sys.foreign_keys there is NO FK constraint at all between aut.AutomationData.
-- SectionID and aut.AutomationDataSections, so deleting a Section that already has
-- saved test data would otherwise silently orphan it forever (invisible in the UI from
-- then on, since the Section dropdown wouldn't list a deleted section, even though the
-- raw rows remain in the table). Matches this project's existing precedent
-- (usp_LoginUserHardDelete's FK-reference guard) - block deletion entirely rather than
-- "warn and proceed anyway".
--
-- Duplicate-section-name prevention (the other gap found alongside this one) is
-- deliberately handled in C# (AutomationRepository) by reusing the already-existing
-- GetAutomationDataSectionsAsync(flowName) - no SQL change needed for that part.

SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE aut.usp_CountAutomationDataForSection
    @SectionID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM aut.AutomationData WHERE SectionID = @SectionID;
END
GO

-- Cascade delete for a Section WITH its saved test data - used only when the user
-- explicitly confirms "delete the section AND its data" from the new UI (the default
-- delete path above still blocks if data exists). Runs both deletes in one
-- transaction so a failure partway through can't leave the section gone but its data
-- orphaned (or vice versa).
CREATE OR ALTER PROCEDURE aut.usp_DeleteAutomationDataSectionCascade
    @SectionID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DELETE FROM aut.AutomationData WHERE SectionID = @SectionID;
        DELETE FROM aut.AutomationDataSections WHERE SectionID = @SectionID;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
