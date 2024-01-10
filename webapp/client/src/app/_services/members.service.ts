import { HttpClient, HttpHeaders } from '@angular/common/http'
import { environment } from 'src/environments/environment'
import { Member } from '../_models/mamber'
import { Injectable } from '@angular/core'
import { map, of } from 'rxjs'

@Injectable({
  providedIn: 'root'
})

export class MembersService {
  baseUrl = environment.apiUrl
  members: Member[] = []

  constructor(private http: HttpClient) { }

  getMembers() {
    if (this.members.length > 0) return of(this.members)
    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(users => {
        this.members = users
        return users
      })
    )
  }

  getMember(username: string) {
    const member = this.members.find(user => user.userName === username)
    if (member) return of(member)
    return this.http.get<Member>(this.baseUrl + 'users/username/' + username)
  }

  updateProfile(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(_ => {
        const index = this.members.indexOf(member)
        this.members[index] = { ...this.members[index], ...member }
      })
    )
  }

  setMainPhoto(photoId: number) {
    const endpoint = this.baseUrl + 'users/set-main-photo/' + photoId
    return this.http.put(endpoint, {})
  }
  deletePhoto(photoId: number) {
    const endpoint = this.baseUrl + 'users/delete-photo/' + photoId
    return this.http.delete(endpoint)
  }
}






