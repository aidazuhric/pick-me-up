import '../models/recommendation.dart';
import 'base_provider.dart';

class RecommendationProvider extends BaseProvider<Recommendation> {
  RecommendationProvider() : super('Recommendations');

  @override
  Recommendation fromJson(dynamic data) =>
      Recommendation.fromJson(data as Map<String, dynamic>);
}
