// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'support_ticket.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SupportTicketReply _$SupportTicketReplyFromJson(Map<String, dynamic> json) =>
    SupportTicketReply(
      id: (json['id'] as num?)?.toInt(),
      authorName: json['authorName'] as String?,
      body: json['body'] as String?,
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
    );

Map<String, dynamic> _$SupportTicketReplyToJson(SupportTicketReply instance) =>
    <String, dynamic>{
      'id': instance.id,
      'authorName': instance.authorName,
      'body': instance.body,
      'createdAt': instance.createdAt?.toIso8601String(),
    };

SupportTicket _$SupportTicketFromJson(Map<String, dynamic> json) =>
    SupportTicket(
      id: (json['id'] as num?)?.toInt(),
      submitterName: json['submitterName'] as String?,
      subject: json['subject'] as String?,
      body: json['body'] as String?,
      status: json['status'] as String?,
      targetUserName: json['targetUserName'] as String?,
      resolvedByName: json['resolvedByName'] as String?,
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
      resolvedAt: json['resolvedAt'] == null
          ? null
          : DateTime.parse(json['resolvedAt'] as String),
      replies: (json['replies'] as List<dynamic>?)
          ?.map((e) => SupportTicketReply.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$SupportTicketToJson(SupportTicket instance) =>
    <String, dynamic>{
      'id': instance.id,
      'submitterName': instance.submitterName,
      'subject': instance.subject,
      'body': instance.body,
      'status': instance.status,
      'targetUserName': instance.targetUserName,
      'resolvedByName': instance.resolvedByName,
      'createdAt': instance.createdAt?.toIso8601String(),
      'resolvedAt': instance.resolvedAt?.toIso8601String(),
      'replies': instance.replies,
    };
