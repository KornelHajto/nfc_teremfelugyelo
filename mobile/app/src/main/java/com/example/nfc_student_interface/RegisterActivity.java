package com.example.nfc_student_interface;

import android.app.Activity;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.provider.MediaStore;
import android.text.InputType;
import android.util.Base64;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.EdgeToEdge;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import android.Manifest;

import java.io.ByteArrayOutputStream;

interface RegisterCallback {
    void onPictureTaken();
    void onCancelled();
}

public class RegisterActivity extends AppCompatActivity {

    private EditText etNeptun;
    private EditText etUsername;
    private EditText etPassword;
    private EditText etPasswordAgain;

    private ImageButton btnTogglePassword;
    private ImageButton btnTogglePasswordAgain;
    private Button btnRegister;

    private TextView tvLoginLink;

    private boolean showPsw = false;
    private boolean showPswAgain = false;
    private boolean pictueTaken = false;

    private String base64String = "";
    private static final int CAMERA_PERMISSION_CODE = 100;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.register_activity);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main), (v, insets) -> {
            Insets systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom);
            return insets;
        });

        checkCameraPermission();

        initializeActivity();
    }

    private void initializeActivity(){
        etNeptun = findViewById(R.id.etNeptun);
        etUsername = findViewById(R.id.etUsername);
        etPassword = findViewById(R.id.etPassword);
        etPasswordAgain = findViewById(R.id.etPasswordAgain);
        btnRegister = findViewById(R.id.btnRegister);
        btnTogglePassword = findViewById(R.id.btnTogglePassword);
        btnTogglePasswordAgain = findViewById(R.id.btnTogglePasswordAgain);
        tvLoginLink = findViewById(R.id.tvLoginLink);

        tvLoginLink.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(RegisterActivity.this, LoginActivity.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_NO_ANIMATION);
                startActivity(intent);
                finish();
            }
        });

        btnTogglePassword.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                showPsw = !showPsw;
                if (showPsw) {
                    etPassword.setInputType(InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD);
                } else {
                    etPassword.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
                }
                etPassword.setSelection(etPassword.length());
            }
        });

        btnTogglePasswordAgain.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                showPswAgain = !showPswAgain;
                if (showPswAgain) {
                    etPasswordAgain.setInputType(InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD);
                } else {
                    etPasswordAgain.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
                }
                etPassword.setSelection(etPassword.length());
            }
        });

        btnRegister.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                startRegisterProcess(new RegisterCallback() {
                    @Override
                    public void onPictureTaken() {
                        goHome(); // continue to home when done
                    }

                    @Override
                    public void onCancelled() {
                        // optional: handle cancel case
                    }
                });
            }
        });

    }


    private void startRegisterProcess(RegisterCallback callback) {
        new AlertDialog.Builder(this)
                .setTitle("Fénykép készítése")
                .setMessage("A regisztráció folytatásához képet készítünk az arcodról. Folytatod?")
                .setPositiveButton("OK", (dialog, which) -> {
                    takePicture(callback); // Pass callback to chain async flow
                })
                .setNegativeButton("Mégse", (dialog, which) -> {
                    dialog.dismiss();
                    Toast.makeText(this, "Regisztráció megszakítva", Toast.LENGTH_SHORT).show();
                    callback.onCancelled();
                })
                .setCancelable(false)
                .show();
    }

    private void askRepeat(RegisterCallback callback) {
        new AlertDialog.Builder(this)
                .setTitle("Új fénykép")
                .setMessage("Szeretnél másik képet csinálni?")
                .setPositiveButton("Igen", (dialog, which) -> {
                    startRegisterProcess(callback); // restart the process
                })
                .setNegativeButton("Nem", (dialog, which) -> {
                    dialog.dismiss();
                    Toast.makeText(this, "Regisztráció folytatása", Toast.LENGTH_SHORT).show();
                    callback.onPictureTaken(); // continue to next step
                })
                .setCancelable(false)
                .show();
    }

    private void takePicture(RegisterCallback callback) {
        Toast.makeText(this, "Camera megnyitása...", Toast.LENGTH_SHORT).show();
        Intent intent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
        imgCaptureResultLauncher.launch(intent);

        // Store the callback for later (see below)
        this.pendingCallback = callback;
    }

    // Hold callback reference
    private RegisterCallback pendingCallback;

    ActivityResultLauncher<Intent> imgCaptureResultLauncher = registerForActivityResult(
            new ActivityResultContracts.StartActivityForResult(),
            result -> {
                if (result.getResultCode() == Activity.RESULT_OK) {
                    Intent data = result.getData();
                    if (data != null) {
                        handleData(data);

                        // After successful picture → ask to retake
                        if (pendingCallback != null)
                            askRepeat(pendingCallback);
                    }
                } else {
                    Toast.makeText(this, "Kamera megszakítva", Toast.LENGTH_SHORT).show();
                    if (pendingCallback != null)
                        pendingCallback.onCancelled();
                }
            }
    );

    public void handleData(Intent intentData){
        Bundle bundle = intentData.getExtras();
        if (bundle != null) {
            Bitmap bitmap = (Bitmap) bundle.get("image"); // replace "image" with your key
            if (bitmap != null) {
                // Convert Bitmap to Base64
                ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
                bitmap.compress(Bitmap.CompressFormat.PNG, 100, outputStream);
                byte[] byteArray = outputStream.toByteArray();
                base64String = Base64.encodeToString(byteArray, Base64.DEFAULT);
            }
        }
    }

    private void goHome(){
        Intent intent = new Intent(RegisterActivity.this, HomeActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_NO_ANIMATION);
        startActivity(intent);
        finish();
    }

    private void checkCameraPermission() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA)
                != PackageManager.PERMISSION_GRANTED) {
            // Permission not granted, request it
            ActivityCompat.requestPermissions(this,
                    new String[]{Manifest.permission.CAMERA},
                    CAMERA_PERMISSION_CODE);
        }
    }

    // Handle permission result
    @Override
    public void onRequestPermissionsResult(int requestCode,
                                           @NonNull String[] permissions,
                                           @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == CAMERA_PERMISSION_CODE) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
            } else {
                Toast.makeText(this, "Camera permission denied", Toast.LENGTH_SHORT).show();
            }
        }
    }
}

