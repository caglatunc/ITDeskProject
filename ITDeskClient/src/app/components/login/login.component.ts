import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { FormsModule } from '@angular/forms';
import { DividerModule } from 'primeng/divider';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { LoginModel } from 'src/app/models/login.model';
import { CheckboxModule } from 'primeng/checkbox';
import { GoogleSigninButtonModule, SocialAuthService } from '@abacritt/angularx-social-login';
import { ErrorService } from 'src/app/services/error.service';
import { HttpService } from 'src/app/services/http.service';



@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    FormsModule,
    DividerModule,
    ToastModule,
    CheckboxModule,
    GoogleSigninButtonModule
  ],
   templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export default class LoginComponent implements OnInit {
  request: LoginModel = new LoginModel();

  constructor(
    private message: MessageService,
    private http: HttpService,
    private router: Router,
    private error: ErrorService,
    private auth: SocialAuthService) { }
    
  ngOnInit(): void {
    this.auth.authState.subscribe(res => {
      this.http.post("Auth/GoogleLogin", res, (data)=>{
        localStorage.setItem("response", JSON.stringify(data));
        this.router.navigateByUrl("/");
      })
      console.log(res);
    })
  }

  signIn() {
    if (this.request.userNameOrEmail.length < 3) {
      this.message.add({ severity: 'warn', summary: 'Validasyon Hatası', detail: 'Geçerli bir kullanıcı adı veya email giriniz!' });
      return;
    }
    if (this.request.password.length < 6) {
      this.message.add({ severity: 'warn', summary: 'Validasyon Hatası', detail: 'Şifreniz en az altı karakter olmalıdır!' });
      return;
    }

    this.http.post("Auth/Login", this.request, res=>{
      localStorage.setItem("response", JSON.stringify(res));
      this.router.navigateByUrl("/");
    });
  }
}
