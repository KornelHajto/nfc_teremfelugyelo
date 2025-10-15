package com.example.nfc_student_interface;

import androidx.room.RoomDatabase;

import com.example.nfc_student_interface.DAO.Attendance;
import com.example.nfc_student_interface.DAO.AttendanceDao;
import com.example.nfc_student_interface.DAO.Course;
import com.example.nfc_student_interface.DAO.CourseDao;
import com.example.nfc_student_interface.DAO.Notification;
import com.example.nfc_student_interface.DAO.NotificationDao;
import com.example.nfc_student_interface.DAO.User;
import com.example.nfc_student_interface.DAO.UserDao;

@androidx.room.Database(entities = {Course.class, User.class, Notification.class, Attendance.class}, version = 1)
public abstract class Database extends RoomDatabase {
    public abstract CourseDao courseDao();
    public abstract UserDao userDao();
    public abstract NotificationDao notificationDao();
    public abstract AttendanceDao attendanceDao();
}
