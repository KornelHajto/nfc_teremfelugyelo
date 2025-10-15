package com.example.nfc_student_interface.DAO;

import androidx.annotation.NonNull;
import androidx.room.Entity;
import androidx.room.Ignore;
import androidx.room.PrimaryKey;
import androidx.room.TypeConverters;

import com.example.nfc_student_interface.StringListConverter;

import java.util.ArrayList;
import java.util.List;

@Entity(tableName = "courses")
public class Course {

    @PrimaryKey
    private int id;

    private String name;
    private String subject;
    private String classroom;
    private String duration;

    // Store date list as a single string or use a converter
    @TypeConverters(StringListConverter.class)
    private List<String> date;

    @Ignore
    public Course() {
        date = new ArrayList<>();
    }

    public Course(int id, String name, String subject, String classroom, List<String> date, String duration) {
        this.id = id;
        this.name = name;
        this.subject = subject;
        this.classroom = classroom;
        this.date = date;
        this.duration = duration;
    }

    // Getters
    public int getId() { return id; }

    public String getName() { return name; }

    public String getSubject() { return subject; }

    public String getClassroom() { return classroom; }

    public List<String> getDate() { return date; }

    public String getDuration() { return duration; }

    // Setters
    public void setId(int id) { this.id = id; }

    public void setName(String name) { this.name = name; }

    public void setSubject(String subject) { this.subject = subject; }

    public void setClassroom(String classroom) { this.classroom = classroom; }

    public void setDate(List<String> date) { this.date = date; }

    public void setDuration(String duration) { this.duration = duration; }

    @NonNull
    @Override
    public String toString() {
        return "{" +
                "id=" + id +
                ", name='" + name + '\'' +
                ", subject='" + subject + '\'' +
                ", classroom='" + classroom + '\'' +
                ", duration='" + duration + '\'' +
                ", date=" + date +
                '}';
    }
}
