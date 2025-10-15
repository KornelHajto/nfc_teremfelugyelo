package com.example.nfc_student_interface;

import android.content.Context;

import androidx.annotation.NonNull;
import androidx.room.Room;
import androidx.room.RoomDatabase;
import androidx.sqlite.db.SupportSQLiteDatabase;

import com.example.nfc_student_interface.DAO.AttendanceDao;
import com.example.nfc_student_interface.DAO.CourseDao;
import com.example.nfc_student_interface.DAO.NotificationDao;
import com.example.nfc_student_interface.DAO.User;
import com.example.nfc_student_interface.DAO.UserDao;

import java.util.List;

public class DatabaseServiceManager {
    Database db;
    Context _context;

    UserDao userDao;
    AttendanceDao attendanceDao;
    CourseDao courseDao;
    NotificationDao notificationDao;

    public DatabaseServiceManager(Context context){
        _context = context;
        RoomDatabase.Callback myCallback = new RoomDatabase.Callback() {
            @Override
            public void onCreate(@NonNull SupportSQLiteDatabase db) {
                super.onCreate(db);
            }

            @Override
            public void onOpen(@NonNull SupportSQLiteDatabase db) {
                super.onOpen(db);
            }
        };
        db = Room.databaseBuilder(_context, Database.class, "SunScopeDB").addCallback(myCallback).fallbackToDestructiveMigration().allowMainThreadQueries().build();
        userDao = db.userDao();
        attendanceDao = db.attendanceDao();
        courseDao = db.courseDao();
        notificationDao = db.notificationDao();
    }

    public void addUser(User user){
        userDao.addUser(user);
    }

    public void updateUser(User user){
        userDao.updateUser(user);
    }

    public User getUser(String code){
        return userDao.getUserByCode(code);
    }

    public User getActive(){
        List<User> users = userDao.getAll();
        for (User u: users) {
            if(u.getToken() == null){
                return u;
            }
        }
        return null;
    }

    public void updateUserOnLogout(String username){
        User u = getUser(username);
        u.setToken(null);

    }

    public boolean checkForActive(){
        List<User> users = userDao.getAll();
        for (User u: users) {
            if(u.getToken() != null && !u.getToken().equals("")){
                return true;
            }
        }
        return false;
    }

    public void deleteUser(String code){
        userDao.delete(userDao.getUserByCode(code));
    }

    public String getToken(){
        User u = getActive();
        if(u == null){
            return "";
        }
        return  u.getToken();
    }
}
