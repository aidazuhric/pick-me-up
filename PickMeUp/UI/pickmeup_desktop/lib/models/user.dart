import 'package:json_annotation/json_annotation.dart';

part 'user.g.dart';

@JsonSerializable()
class User {
  int? id;
  String? firstName;
  String? lastName;
  String? email;
  String? username;
  String? role;
  bool? isActive;
  DateTime? createdAt;
  DateTime? lastLoginAt;
  DateTime? updatedAt;
  String? phoneNumber;
  String? profileImageUrl;

  User({
    this.id,
    this.firstName,
    this.lastName,
    this.email,
    this.username,
    this.role,
    this.isActive,
    this.createdAt,
    this.lastLoginAt,
    this.updatedAt,
    this.phoneNumber,
    this.profileImageUrl,
  });

  String get fullName => [firstName, lastName]
      .where((part) => part != null && part.isNotEmpty)
      .join(' ');

  factory User.fromJson(Map<String, dynamic> json) => _$UserFromJson(json);

  Map<String, dynamic> toJson() => _$UserToJson(this);
}
