import { Component, Input } from '@angular/core'
import { faUser, faHeart, faEnvelope } from '@fortawesome/free-regular-svg-icons'
import { Member } from 'src/app/_models/mamber'


@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent {
  faUser = faUser
  faEnvelope = faEnvelope
  faHeart = faHeart

  @Input() member: Member | undefined

}
