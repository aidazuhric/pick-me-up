import 'package:json_annotation/json_annotation.dart';

part 'reference_item.g.dart';

@JsonSerializable()
class ReferenceItem {
  int? id;
  String? name;
  String? description;
  String? postalCode;
  int? sortOrder;
  bool? isActive;

  ReferenceItem({
    this.id,
    this.name,
    this.description,
    this.postalCode,
    this.sortOrder,
    this.isActive,
  });

  factory ReferenceItem.fromJson(Map<String, dynamic> json) =>
      _$ReferenceItemFromJson(json);

  Map<String, dynamic> toJson() => _$ReferenceItemToJson(this);
}
