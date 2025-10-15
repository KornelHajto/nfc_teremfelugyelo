package com.example.nfc_student_interface.DAO;

import androidx.room.Dao;
import androidx.room.Delete;
import androidx.room.Insert;
import androidx.room.OnConflictStrategy;
import androidx.room.Query;
import androidx.room.Update;

import java.util.List;

@Dao
public interface NotificationDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    void insertAll(List<Notification> notifications);

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    void addNotification(Notification notification);

    @Delete
    void delete(Notification notification);

    @Update
    void updateNotification(Notification notification);

    @Query("SELECT * FROM notifications")
    List<Notification> getAll();

    @Query("SELECT * FROM notifications WHERE id == :id LIMIT 1")
    Notification getNotification(int id);

    @Query("SELECT * FROM notifications WHERE classroom == :classroom")
    List<Notification> getNotificationsByClassroom(String classroom);

    @Query("DELETE FROM notifications")
    void deleteAll();
}
