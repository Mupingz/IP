import { faUser,faHeart,faEnvelope } from '@fortawesome/free-regular-svg-icons'
import { Component, Input, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { CommonModule } from '@angular/common';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule } from 'ng-gallery';
import { MembersService } from 'src/app/_services/members.service';
import { ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { PresenceService } from 'src/app/_services/presence.service';


@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
  
})
export class MemberCardComponent {
  faUser = faUser
  faEnvelope = faEnvelope
  faHeart = faHeart
  @Input() member: Member | undefined

  constructor(private memberService: MembersService, private toastr: ToastrService, public presenceService: PresenceService) { }

  addLike(member: Member) {
    this.memberService.addLike(member.userName).subscribe({
      next: _ => this.toastr.success(`You have liked ${member.userName}`)
    })
  }
}