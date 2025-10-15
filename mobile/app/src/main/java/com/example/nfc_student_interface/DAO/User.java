package com.example.nfc_student_interface.DAO;

import androidx.annotation.NonNull;
import androidx.room.Entity;
import androidx.room.PrimaryKey;

@Entity(tableName = "users")
public class User {
    @NonNull
    @PrimaryKey(autoGenerate = false)
    private String code;
    private String name;
    private String token;

    public User() {}

    public User(String name, String code, String token) {
        this.name = name;
        this.code = code;
        this.token = token;
    }

    public String getName() { return name; }
    public String getCode() { return code; }
    public String getToken() { return token; }

    public void setName(String name) { this.name = name; }
    public void setCode(String code) { this.code = code; }
    public void setToken(String token) { this.token = token; }

    @NonNull
    @Override
    public String toString() {
        return "User{" +
                ", name='" + name + '\'' +
                ", code='" + code + '\'' +
                ", token='" + token + '\'' +
                '}';
    }
}
