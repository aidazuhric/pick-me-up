import 'package:json_annotation/json_annotation.dart';

part 'support_ticket.g.dart';

@JsonSerializable()
class SupportTicketReply {
  int? id;
  String? authorName;
  String? body;
  DateTime? createdAt;

  SupportTicketReply({this.id, this.authorName, this.body, this.createdAt});

  factory SupportTicketReply.fromJson(Map<String, dynamic> json) =>
      _$SupportTicketReplyFromJson(json);

  Map<String, dynamic> toJson() => _$SupportTicketReplyToJson(this);
}

@JsonSerializable()
class SupportTicket {
  int? id;
  String? submitterName;
  String? subject;
  String? body;
  String? status;
  String? targetUserName;
  String? resolvedByName;
  DateTime? createdAt;
  DateTime? resolvedAt;
  List<SupportTicketReply>? replies;

  SupportTicket({
    this.id,
    this.submitterName,
    this.subject,
    this.body,
    this.status,
    this.targetUserName,
    this.resolvedByName,
    this.createdAt,
    this.resolvedAt,
    this.replies,
  });

  factory SupportTicket.fromJson(Map<String, dynamic> json) =>
      _$SupportTicketFromJson(json);

  Map<String, dynamic> toJson() => _$SupportTicketToJson(this);
}
