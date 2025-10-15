package com.example.nfc_student_interface.DAO;

import androidx.annotation.NonNull;
import androidx.room.Entity;
import androidx.room.PrimaryKey;

@Entity(tableName = "attendances")
public class Attendance {
    @PrimaryKey(autoGenerate = true)
    private int id;
    private String course;
    private String date;
    private boolean excused = false;

    public Attendance() {}

    public Attendance(String course, String date, int status, boolean exam) {
        this.course = course;
        this.date = date;
        switch(status){
            case 3: excused = true; break;
            case 4: excused = false; break;
        }
        if(exam && status==2) excused = false;
    }

    public int getId() {
        return id;
    }

    public String getCourse() {
        return course;
    }

    public String getDate() {
        return date;
    }

    public boolean isExcused() {
        return excused;
    }

    // Setters
    public void setId(int id) {
        this.id = id;
    }

    public void setCourse(String course) {
        this.course = course;
    }

    public void setDate(String date) {
        this.date = date;
    }

    public void setExcused(boolean excused) {
        this.excused = excused;
    }

    @Override
    @NonNull
    public String toString() {
        return "Attendance{" +
                "id=" + id +
                ", course='" + course + '\'' +
                ", date='" + date + '\'' +
                ", excused=" + excused +
                '}';
    }
}
