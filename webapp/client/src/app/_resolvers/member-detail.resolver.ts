import { ResolveFn } from '@angular/router';
import { MembersService } from '../_services/members.service';
import { Member } from '../_models/member';
import { inject } from '@angular/core';

export const memberDetailResolver: ResolveFn<Member> = (route, state) => {
  const memberService = inject(MembersService)
  return memberService.getMember(route.paramMap.get('username')!)
}