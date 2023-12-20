import { HttpClient, HttpHeaders } from '@angular/common/http'
import { environment } from 'src/environments/environment'
import { Member } from '../_models/mamber'
import { Injectable } from '@angular/core'

@Injectable({
  providedIn: 'root'
})

export class MembersService {
  baseUrl = environment.apiUrl

  constructor(private http: HttpClient) { }


  getMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'users')
  }

  getMember(username: string) {
    return this.http.get<Member>(this.baseUrl + 'users/username/' + username)
  }
}




