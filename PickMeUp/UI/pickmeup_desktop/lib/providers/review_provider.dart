import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/review.dart';
import 'base_provider.dart';

class ReviewProvider extends BaseProvider<Review> {
  ReviewProvider() : super('Reviews');

  @override
  Review fromJson(dynamic data) => Review.fromJson(data as Map<String, dynamic>);

  Future<List<RatingSummary>> companyAverages() => _averages('CompanyAverages');

  Future<List<RatingSummary>> driverAverages() => _averages('DriverAverages');

  Future<List<RatingSummary>> _averages(String path) async {
    final response = await http.get(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$path'),
      headers: createHeaders(),
    );

    validateResponse(response);

    return (jsonDecode(response.body) as List)
        .map((e) => RatingSummary.fromJson(e as Map<String, dynamic>))
        .toList();
  }
}
