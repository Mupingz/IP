import { Component, HostListener, OnInit, ViewChild } from '@angular/core'
import { NgForm } from '@angular/forms'
import { ToastrService } from 'ngx-toastr'
import { take } from 'rxjs'
import { Member } from 'src/app/_models/mamber'
import { User } from 'src/app/_models/user'
import { AccountService } from 'src/app/_services/account.service'
import { MembersService } from 'src/app/_services/members.service'

@Component({
  selector: 'app-member-profile',
  templateUrl: './member-profile.component.html',
  styleUrls: ['./member-profile.component.css']
})
export class MemberProfileComponent implements OnInit {

  @ViewChild('profileForm') profileForm: NgForm | undefined
  member: Member | undefined
  user: User | undefined | null

  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    if (this.profileForm?.dirty) {
      $event.returnValue = true
    }
  }

  constructor(private toastrService: ToastrService, private accountService: AccountService, private memberService: MembersService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => this.user = user
    })
  }

  ngOnInit(): void {
    this.loadMember()
  }

  loadMember() {
    if (!this.user) return
    this.memberService.getMember(this.user.username).subscribe({
      next: user => this.member = user
    })
  }

  updateProfile() {
    this.memberService.updateProfile(this.profileForm?.value).subscribe({
      next: _ => {
        this.toastrService.success('Profile updated !!')
        this.profileForm?.reset(this.member)
      }
    })
  }
}
