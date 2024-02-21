import { CommonModule } from '@angular/common'
import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core'
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome'
import { GalleryModule } from 'ng-gallery'
import { TabsModule } from 'ngx-bootstrap/tabs'
import { TimeagoModule } from 'ngx-timeago'
import { Message } from 'src/app/_models/message'
import { MessageService } from 'src/app/_services/message.service'
import { faClock, faPaperPlane } from '@fortawesome/free-regular-svg-icons'
import { FormsModule, NgForm } from '@angular/forms'
import { NgxLongPress2Module } from 'ngx-long-press2'


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  standalone: true, //เพราะจะเอาไปใช้ใน member-detail.component ซึ่งเป็น component แบบ standalone
  imports: [CommonModule, FontAwesomeModule, TimeagoModule, FormsModule, NgxLongPress2Module],
})
export class MemberMessagesComponent implements OnInit {
  @Input() username?: string
  //@Input() messages: Message[] = []
  @ViewChild('messageForm') messageForm?: NgForm
  messageContent = ''

  faClock = faClock
  faPaperPlane = faPaperPlane

  constructor(public messageService: MessageService) { }

  onLongPressMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe({
      //next: _ => this.messages?.splice(this.messages.findIndex(ms => ms.id === id), 1)
    })
  }

  sendMessage() {
    if (!this.username) return

    this.messageService.sendMessage(this.username, this.messageContent)?.then
      (() => {
        this.messageForm?.reset()
      })

    // this.messageService.sendMessage(this.username, this.messageContent).subscribe({
    //   next: response => {
    //     //this.messages.push(response)
    //     //this.messageForm?.reset()
    //   }
    // })
  }

  ngOnInit(): void {
    //this.loadMessages()
  }

  loadMessages() {
    if (!this.username) return
    this.messageService.getMessagesThread(this.username).subscribe({
      //next: response => this.messages = response
    })
  }

}
