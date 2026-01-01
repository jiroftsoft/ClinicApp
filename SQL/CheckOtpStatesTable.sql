-- Quick check if OtpStates table exists
IF OBJECT_ID('dbo.OtpStates', 'U') IS NOT NULL
BEGIN
    PRINT 'OtpStates table EXISTS'
    
    -- Check if it has data
    DECLARE @count INT;
    SELECT @count = COUNT(*) FROM dbo.OtpStates;
    PRINT 'OtpStates record count: ' + CAST(@count AS NVARCHAR(10));
    
    -- Show structure
    EXEC sp_help 'dbo.OtpStates';
END
ELSE
BEGIN
    PRINT 'ERROR: OtpStates table does NOT exist!'
    PRINT 'Migration may have failed.'
END;

