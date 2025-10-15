package com.example.nfc_student_interface;

import android.util.Log;

import com.example.nfc_student_interface.DAO.Course;
import com.example.nfc_student_interface.DAO.CourseDao;
import com.example.nfc_student_interface.DAO.Notification;
import com.example.nfc_student_interface.DAO.NotificationDao;
import com.example.nfc_student_interface.DAO.User;
import com.example.nfc_student_interface.DAO.UserDao;
import com.google.firebase.crashlytics.buildtools.reloc.com.google.common.net.MediaType;
import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList;
import java.util.List;

public class ApiHandler {

    private String baseLink = "http://192.168.153.78:5189";

    private String login = "/api/Users/login";
    private String register = "/api/Users/create";
    private String course = "/api/Courses/user/getall";
    private String exam = "/api/Exams/user/getall"; //!!!
    private String notification = "/api/Logs/user/getall";
    private String attendance = "/api/Attendances/getall";
    private String attendanceExam = "/api/ExamAttendances/user/getall";

    public ApiHandler(){

    }

    public void Login(String neptunid, String password, DatabaseServiceManager dbManager) {
        new Thread(() -> {
            try {
                URL url = new URL(baseLink + login);
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                conn.setRequestMethod("POST");
                conn.setRequestProperty("Accept", "application/json");
                conn.setRequestProperty("Content-Type", "application/json; charset=UTF-8");
                conn.setDoOutput(true);

                // Build JSON body
                JSONObject jsonBody = new JSONObject();
                jsonBody.put("neptunId", neptunid);
                jsonBody.put("password", password);

                // Send body
                OutputStream os = conn.getOutputStream();
                byte[] input = jsonBody.toString().getBytes("utf-8");
                os.write(input, 0, input.length);
                os.flush();
                os.close();

                // Get response
                int code = conn.getResponseCode();
                if (code == HttpURLConnection.HTTP_OK) {
                    InputStream is = conn.getInputStream();
                    BufferedReader reader = new BufferedReader(new InputStreamReader(is));
                    StringBuilder responseBuilder = new StringBuilder();
                    String line;
                    while ((line = reader.readLine()) != null) {
                        responseBuilder.append(line);
                    }
                    reader.close();

                    // Parse JSON
                    String responseString = responseBuilder.toString();
                    JSONObject responseJson = new JSONObject(responseString);

                    String message = responseJson.getString("message");
                    String token = responseJson.getString("token");
                    String name = responseJson.getString("name");

                    if (!token.isEmpty()) {
                        // Insert/update Room database
                        User existingUser = dbManager.getUser(neptunid);

                        User user = new User();
                        user.setCode(neptunid);
                        user.setName(name);
                        user.setToken(token);
                        Log.d("Login", "With token: " + token);

                        if (existingUser == null) {
                            dbManager.addUser(user);
                        } else {
                            dbManager.updateUser(user);
                        }

                        Log.d("Login", "Login successful: " + name);
                    } else {
                        Log.e("Login", "Login failed: " + message);
                    }

                } else {
                    Log.e("Login", "HTTP error code: " + code);
                }

                conn.disconnect();

            } catch (Exception e) {
                e.printStackTrace();
            }
        }).start();
    }

    public void Register(String neptunId, String fullName, String password, String pictureBase64, DatabaseServiceManager dbManager){
        new Thread(() -> {
            try {
                URL url = new URL(baseLink + register);
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                conn.setRequestMethod("POST");
                conn.setRequestProperty("Accept", "application/json");
                conn.setRequestProperty("Content-Type", "application/json; charset=UTF-8");
                conn.setDoOutput(true);

                // Build JSON body
                JSONObject jsonBody = new JSONObject();
                jsonBody.put("neptunId", neptunId);
                jsonBody.put("fullName", fullName);
                jsonBody.put("password", password);
                jsonBody.put("picture", pictureBase64); // base64 string of picture

                // Send body
                OutputStream os = conn.getOutputStream();
                byte[] input = jsonBody.toString().getBytes("utf-8");
                os.write(input, 0, input.length);
                os.flush();
                os.close();

                // Get response
                int code = conn.getResponseCode();
                InputStream is = (code == HttpURLConnection.HTTP_OK) ?
                        conn.getInputStream() : conn.getErrorStream();

                BufferedReader reader = new BufferedReader(new InputStreamReader(is));
                StringBuilder responseBuilder = new StringBuilder();
                String line;
                while ((line = reader.readLine()) != null) {
                    responseBuilder.append(line);
                }
                reader.close();

                // Parse JSON response
                String responseString = responseBuilder.toString();
                JSONObject responseJson = new JSONObject(responseString);
                String message = responseJson.optString("message");

                if (code == HttpURLConnection.HTTP_OK) {
                    Log.d("Register", "Registration successful: " + message);
                } else {
                    Log.e("Register", "Registration failed: " + message);
                }

                conn.disconnect();

            } catch (Exception e) {
                e.printStackTrace();
            }
        }).start();
    }

    public void fetchCourses(DatabaseServiceManager dbManager) {
        new Thread(() -> {
            try {
                URL url = new URL(baseLink + course);
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                conn.setRequestMethod("GET");
                conn.setRequestProperty("Accept", "application/json");
                conn.setRequestProperty("Authorization", "Bearer  "+dbManager.getToken());

                int code = conn.getResponseCode();
                InputStream is = (code == HttpURLConnection.HTTP_OK) ? conn.getInputStream() : conn.getErrorStream();
                BufferedReader reader = new BufferedReader(new InputStreamReader(is));
                StringBuilder responseBuilder = new StringBuilder();
                String line;
                while ((line = reader.readLine()) != null) {
                    responseBuilder.append(line);
                }
                reader.close();

                String responseString = responseBuilder.toString();
                JSONObject responseJson = new JSONObject(responseString);
                String message = responseJson.optString("message");
                Log.d("Courses", "Server message: " + message);

                JSONArray coursesArray = responseJson.getJSONArray("courses");
                List<Course> coursesList = new ArrayList<>();

                for (int i = 0; i < coursesArray.length(); i++) {
                    JSONObject c = coursesArray.getJSONObject(i);

                    int id = c.getInt("id");
                    String name = c.optString("name");
                    String subject = c.optString("subject", null);
                    String classroom = c.optString("classroom", null);
                    String duration = c.optString("duration", null);

                    JSONArray dateArray = c.getJSONArray("date");
                    List<String> dates = new ArrayList<>();
                    for (int j = 0; j < dateArray.length(); j++) {
                        dates.add(dateArray.getString(j));
                    }

                    Course course = new Course(id, name, subject, classroom, dates, duration);
                    coursesList.add(course);
                }

                CourseDao courseDao = dbManager.courseDao;
                // Optionally update Room database
                if (courseDao != null) {
                    for (Course c : coursesList) {
                        Course existing = courseDao.getCourse(c.getId());
                        if (existing == null) {
                            courseDao.addCourse(c);
                        } else {
                            courseDao.updateCourse(c);
                        }
                    }
                }

                conn.disconnect();

            } catch (Exception e) {
                e.printStackTrace();
            }
        }).start();
    }

    public void fetchNotifications(DatabaseServiceManager dbManager) {
        new Thread(() -> {
            try {
                URL url = new URL(baseLink + notification);
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                conn.setRequestMethod("GET");
                conn.setRequestProperty("Accept", "application/json");
                conn.setRequestProperty("Authorization", "Bearer " + dbManager.getToken());

                int code = conn.getResponseCode();
                InputStream is = (code == HttpURLConnection.HTTP_OK) ? conn.getInputStream() : conn.getErrorStream();
                BufferedReader reader = new BufferedReader(new InputStreamReader(is));
                StringBuilder responseBuilder = new StringBuilder();
                String line;
                while ((line = reader.readLine()) != null) {
                    responseBuilder.append(line);
                }
                reader.close();

                String responseString = responseBuilder.toString();
                JSONObject responseJson = new JSONObject(responseString);
                String message = responseJson.optString("message");
                Log.d("Notifications", "Server message: " + message);

                JSONArray logsArray = responseJson.getJSONArray("logs");
                List<Notification> notifications = new ArrayList<>();

                for (int i = 0; i < logsArray.length(); i++) {
                    JSONObject log = logsArray.getJSONObject(i);
                    int id = log.getInt("id");
                    String date = log.optString("date");
                    JSONObject classroomObj = log.optJSONObject("classroom");
                    String roomId = classroomObj != null ? classroomObj.optString("roomId") : null;
                    int enterType = log.optInt("enterType", -1);
                    String comment = log.optString("comment", null);

                    Notification notif = new Notification(roomId, date, comment, enterType);
                    notif.setId(id);
                    notifications.add(notif);
                }

                NotificationDao notificationDao = dbManager.notificationDao;;
                // Optionally update Room database
                if (notificationDao != null) {
                    for (Notification n : notifications) {
                        Notification existing = notificationDao.getNotification(n.getId());
                        if (existing == null) {
                            notificationDao.addNotification(n);
                        } else {
                            notificationDao.updateNotification(n);
                        }
                    }
                }

                conn.disconnect();

            } catch (Exception e) {
                e.printStackTrace();
            }
        }).start();
    }


}
