import 'package:dio/dio.dart';
import '../models/user.dart';
import '../models/login_dto.dart';
import '../models/register_dto.dart';
import '../services/api_client.dart';
import '../services/local_storage.dart';
import 'auth_repository.dart';

class RestAuthRepository implements AuthRepository {
  RestAuthRepository(this._api, this._storage);
  final ApiClient _api;
  final TokenStorage _storage;

  User? _current;

  @override
  Future<User?> currentUser() async {
    if (_current == null) {
      return null;
    }
    final userJson = await _api.get('/me') as Map<String, dynamic>?;
    _current = User(
      id: _current!.id,
      email: (userJson?['email']).toString(),
      displayName: (userJson?['displayName']).toString(),
      roleId: (userJson?['role']?['id']).toString(),
      roleName: (userJson?['role']?['name']).toString(),
    );
    return _current;
  }

  @override
  Future<User> signIn({required String email, required String password}) async {
    final body = LoginDto(email: email, password: password).toJson();
    final data = await _api.post('/auth/login', body: body) as Map<String, dynamic>;

    // Accept several token field names based on backend
    final access = (data['token'])?.toString();
    if (access == null || access.isEmpty) {
      throw DioException.badResponse(
        statusCode: 500,
        requestOptions: RequestOptions(),
        response: Response(requestOptions: RequestOptions(), data: 'Missing access token in /auth/login response'),
      );
    }
    _storage.saveTokens(access);
    final userId = (data['userId']).toString();
    _storage.saveUserId(userId);

    final userJson = await _api.get('/me') as Map<String, dynamic>?;
    _current = User(
      id: userId,
      email: (userJson?['email']).toString(),
      displayName: (userJson?['displayName']).toString(),
      roleId: (userJson?['role']?['id']).toString(),
      roleName: (userJson?['role']?['name']).toString(),
    );
    return _current!;
  }

  @override
  Future<void> signOut() async {
    _current = null;
    _storage.clear();
  }

  @override
  Future<bool> signUp({required String email, required String password, String? displayName}) async {
    final body = RegisterDto(email: email, password: password, displayName: displayName).toJson();
    final data = await _api.post('/auth/register', body: body) as Map<String, dynamic>;

    final userId = (data['userId']).toString();
    return userId.isNotEmpty;
  }
}
