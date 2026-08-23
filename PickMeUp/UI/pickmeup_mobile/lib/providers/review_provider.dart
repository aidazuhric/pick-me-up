import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/rating_summary.dart';
import '../models/review.dart';
import 'base_provider.dart';

class ReviewProvider extends BaseProvider<Review> {
  ReviewProvider() : super('Reviews');

  @override
  Review fromJson(dynamic data) => Review.fromJson(data as Map<String, dynamic>);

  Future<List<RatingSummary>> readDriverAverages() =>
      _readAverages('DriverAverages');

  Future<List<RatingSummary>> readCompanyAverages() =>
      _readAverages('CompanyAverages');

  Future<List<RatingSummary>> _readAverages(String path) async {
    final response = await http.get(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$path'),
      headers: createHeaders(),
    );
    validateResponse(response);

    final decoded = jsonDecode(response.body) as List<dynamic>;

    return decoded
        .map((e) => RatingSummary.fromJson(e as Map<String, dynamic>))
        .toList();
  }
}
