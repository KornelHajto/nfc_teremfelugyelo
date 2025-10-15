package com.example.nfc_student_interface.DAO;

import androidx.room.Dao;
import androidx.room.Delete;
import androidx.room.Insert;
import androidx.room.OnConflictStrategy;
import androidx.room.Query;
import androidx.room.Update;

import java.util.List;

@Dao
public interface CourseDao {

    // Insert a list of courses (replace on conflict)
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    void insertAll(List<Course> courses);

    // Insert a single course (replace on conflict)
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    void addCourse(Course course);

    // Delete a specific course
    @Delete
    void delete(Course course);

    // Update a list of courses
    @Update
    void updateCourses(List<Course> courses);

    // Update a single course
    @Update
    void updateCourse(Course course);

    // Get all courses
    @Query("SELECT * FROM courses")
    List<Course> getAll();

    // Get a course by its ID
    @Query("SELECT * FROM courses WHERE id == :id LIMIT 1")
    Course getCourse(int id);

    // Get all courses that match a given name
    @Query("SELECT * FROM courses WHERE name == :name")
    List<Course> getCoursesByName(String name);

    // Get all courses that match a given subject (nullable-safe)
    @Query("SELECT * FROM courses WHERE subject == :subject")
    List<Course> getCoursesBySubject(String subject);
}
