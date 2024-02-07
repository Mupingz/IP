import { CommonModule } from '@angular/common'
import { Component, OnInit, ViewChild } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery'
import { TabDirective, TabsModule, TabsetComponent } from 'ngx-bootstrap/tabs'
import { TimeagoModule } from 'ngx-timeago'
import { Member } from 'src/app/_models/mamber'
import { MembersService } from 'src/app/_services/members.service'
import { MemberMessagesComponent } from '../member-messages/member-messages.component'
import { Message } from 'src/app/_models/message'
import { MessageService } from 'src/app/_services/message.service'

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
  standalone: true,
  imports: [CommonModule, TabsModule, GalleryModule, TimeagoModule, MemberMessagesComponent,]
})
export class MemberDetailComponent implements OnInit {
  member: Member = {} as Member
  photos: GalleryItem[] = []

  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent
  activeTab?: TabDirective
  messages: Message[] = []


  constructor(private memberService: MembersService, private route: ActivatedRoute, private messageService: MessageService,) { }

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member']
        this.getImages()
      }
    })
    this.route.queryParams.subscribe({
      next: paran => paran['tab'] && this.selectTab(paran['tab'])
    })
  }

  getImages() {
    if (!this.member) return
    for (const photo of this.member.photos) {
      this.photos.push(new ImageItem({ src: photo.url, thumb: photo.url }))
    }
  }



  loadMember() {
    const username = this.route.snapshot.paramMap.get('username')
    if (!username) return
    this.memberService.getMember(username).subscribe({
      next: user => {
        this.member = user
        this.getImages()
      }
    }
    )
  }
  onTabActivated(tab: TabDirective) {
    this.activeTab = tab
    if (this.activeTab.heading === 'Messages') {
      this.loadMessages()
    }
  }
  loadMessages() {
    if (!this.member) return
    this.messageService.getMessagesThread(this.member.userName).subscribe({
      next: response => this.messages = response
    })
  }
  selectTab(tabHeading: string) {
    if (!this.memberTabs) return
    const tab = this.memberTabs.tabs.find(tab => tab.heading === tabHeading)
    if (!tab) return
    tab.active = true
  }


}
