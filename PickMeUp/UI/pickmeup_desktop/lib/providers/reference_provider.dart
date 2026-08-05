import '../models/reference_item.dart';
import 'base_provider.dart';

class ReferenceProvider extends BaseProvider<ReferenceItem> {
  ReferenceProvider(super.endpoint);

  @override
  ReferenceItem fromJson(dynamic data) =>
      ReferenceItem.fromJson(data as Map<String, dynamic>);
}
