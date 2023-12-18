export class TicketModel {
  id:string="";
  subject:string="";
  createdDate:Date= new Date();
  isOpen:boolean=false;
}   

export class TicketDetailModel{
  id:string="";
  content:string="";
  appUserId:string=""; 
  appUser:any;
  createdDate:string="";
}