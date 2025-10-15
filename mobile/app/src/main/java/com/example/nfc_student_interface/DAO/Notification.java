package com.example.nfc_student_interface.DAO;

import androidx.annotation.NonNull;
import androidx.room.Entity;
import androidx.room.PrimaryKey;

@Entity(tableName = "notifications")
public class Notification {
    @PrimaryKey(autoGenerate = true)
    private int id;
    private String classroom;
    private String date;
    private String message;

    public Notification() {}

    public Notification(String room, String date, String message, int enterType) {
        this.classroom = room;
        this.date = date;
        switch(enterType){
            case 0: this.message = "Beléptél a(z) "+room+" terembe"; break;
            case 1: this.message = "Elhagytad a(z) "+room+" teremet"; break;
            case 2: this.message = "Nem lesz órád a(z) "+room+" teremben"; break;
            case 3: this.message = "Belépésed elutasították a(z) "+room+" terembe"; break;
        }
    }

    // Getters and setters
    public int getId() { return id; }
    public String getClassroom() { return classroom; }
    public String getDate() { return date; }
    public String getMessage() { return message; }

    public void setId(int id) { this.id = id; }
    public void setClassroom(String classroom) { this.classroom = classroom; }
    public void setDate(String date) { this.date = date; }
    public void setMessage(String message) { this.message = message; }

    @NonNull
    @Override
    public String toString() {
        return "AttendanceLog{" +
                "id=" + id +
                ", classroom='" + classroom + '\'' +
                ", date='" + date + '\'' +
                ", message='" + message + '\'' +
                '}';
    }
}
