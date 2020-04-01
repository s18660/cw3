create procedure promoteStudents
    @studiesName varchar(100),
    @semester int
as
begin
    declare @idStudy int;
    declare @idEnrollmentCount int;
    declare @idEnrollmentCurrent int;
    declare @idEnrollmentFuture int;
    select @idStudy = idstudy from Studies where Name = @studiesName;
    
    select @idEnrollmentCurrent = idenrollment from Enrollment where Semester = @Semester and IdStudy = @idStudy

    select @idEnrollmentCount = count(idenrollment) from Enrollment where Semester = @Semester+1 and IdStudy = @idStudy
    if @idEnrollmentCount = 0 
        begin
            insert into Enrollment select max(idenrollment)+1, @Semester+1, @idStudy, getdate() from Enrollment
        end;
    
    select @idEnrollmentFuture =  idenrollment from Enrollment where Semester = @Semester + 1 and IdStudy = @idStudy

    update Student set idEnrollment = @idEnrollmentFuture where idEnrollment = @idEnrollmentCurrent;
end

Exec promoteStudents 'Informatyka dzienne', 1
