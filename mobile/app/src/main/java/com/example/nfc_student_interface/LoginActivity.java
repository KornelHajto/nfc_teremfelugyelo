package com.example.nfc_student_interface;

import android.content.Intent;
import android.os.Bundle;
import android.text.InputType;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.EdgeToEdge;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.example.nfc_student_interface.DAO.User;

public class LoginActivity extends AppCompatActivity {

    private EditText etNeptun;
    private EditText etPassword;
    private ImageButton btnTogglePassword;
    private Button btnLogin;
    private TextView tvSignupLink;

    private boolean showPsw = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.login_activity);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main), (v, insets) -> {
            Insets systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom);
            return insets;
        });

        initializeActivity();

        checkforActive();
    }

    private void checkforActive(){
        DatabaseServiceManager db = new DatabaseServiceManager(getApplicationContext());
        if(db.checkForActive() && db.getActive() != null){
            goHome();
        }
    }

    private void initializeActivity(){
        etNeptun = findViewById(R.id.etNeptun);
        etPassword = findViewById(R.id.etPassword);
        btnTogglePassword = findViewById(R.id.btnTogglePassword);
        btnLogin = findViewById(R.id.btnLogin);
        tvSignupLink = findViewById(R.id.tvSignupLink);

        tvSignupLink.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(LoginActivity.this, RegisterActivity.class);
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

        btnLogin.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                startLoginProcess();
            }
        });
    }

    private void startLoginProcess() {
        String neptuncode = etNeptun.getText().toString();
        String password = etPassword.getText().toString(); // corrected to password field
        ApiHandler api = new ApiHandler();
        DatabaseServiceManager db = new DatabaseServiceManager(getApplicationContext());

//        // Call the login asynchronously
//        new Thread(() -> {
//            api.Login(neptuncode, password, db);
//            //Log.e("Login", "Login success: " + u.getToken());
//        }).start();
//        while(!db.checkForActive()){
//
//        }
        goHome();
    }

    private void goHome(){
        Intent intent = new Intent(LoginActivity.this, HomeActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_NO_ANIMATION);
        startActivity(intent);
        finish();
    }

}