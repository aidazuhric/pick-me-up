import 'package:json_annotation/json_annotation.dart';

part 'taxi_company_details.g.dart';

@JsonSerializable()
class TaxiCompanyDetails {
  int? id;
  String? name;
  String? cityName;
  String? hqAddress;
  String? contactPhone;
  double? startingPrice;
  double? pricePerKm;
  String? logoImageUrl;
  int? vehicleCount;
  double? averageRating;
  int? reviewCount;

  TaxiCompanyDetails({
    this.id,
    this.name,
    this.cityName,
    this.hqAddress,
    this.contactPhone,
    this.startingPrice,
    this.pricePerKm,
    this.logoImageUrl,
    this.vehicleCount,
    this.averageRating,
    this.reviewCount,
  });

  factory TaxiCompanyDetails.fromJson(Map<String, dynamic> json) =>
      _$TaxiCompanyDetailsFromJson(json);

  Map<String, dynamic> toJson() => _$TaxiCompanyDetailsToJson(this);
}
