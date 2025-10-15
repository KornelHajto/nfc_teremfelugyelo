package com.example.nfc_student_interface.DAO;


import androidx.room.Dao;
import androidx.room.Delete;
import androidx.room.Insert;
import androidx.room.OnConflictStrategy;
import androidx.room.Query;
import androidx.room.Update;

import java.util.List;

@Dao
public interface AttendanceDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    void insertAll(List<Attendance> attendances);

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    void addAttendance(Attendance attendance);

    @Delete
    void delete(Attendance attendance);

    @Update
    void updateAttendance(Attendance attendance);

    @Query("SELECT * FROM attendances")
    List<Attendance> getAll();

    @Query("SELECT * FROM attendances WHERE id == :id LIMIT 1")
    Attendance getAttendance(int id);

    @Query("SELECT * FROM attendances WHERE course == :course")
    List<Attendance> getAttendancesByCourse(String course);

    @Query("DELETE FROM attendances")
    void deleteAll();
}
