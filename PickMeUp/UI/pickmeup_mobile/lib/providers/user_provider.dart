import '../models/user.dart';
import 'base_provider.dart';

class UserProvider extends BaseProvider<User> {
  UserProvider() : super('Users');

  @override
  User fromJson(dynamic data) => User.fromJson(data as Map<String, dynamic>);
}
