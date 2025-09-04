import 'dart:convert';
import 'dart:io';

import 'package:http/http.dart' as http;

class ApiClient {
  ApiClient({http.Client? httpClient, String? baseUrl})
    : _client = httpClient ?? http.Client(),
      // Platform-specific default base URL:
      // - Android emulator uses special host 10.0.2.2
      // - macOS (and iOS simulator) often fail with `localhost` if server binds IPv4 only, so use 127.0.0.1
      // - Other platforms fallback to localhost
      baseUrl =
          baseUrl ??
          (Platform.isAndroid
              ? 'http://10.0.2.2:5247'
              : (Platform.isMacOS || Platform.isIOS)
              ? 'http://127.0.0.1:5247'
              : 'http://localhost:5247');

  final http.Client _client;
  final String baseUrl;

  Duration timeout = const Duration(seconds: 15);

  Future<String> login({required String email, required String password}) async {
    final uri = Uri.parse('$baseUrl/auth/login');
    try {
      final response = await _client
          .post(
            uri,
            headers: {HttpHeaders.contentTypeHeader: 'application/json', HttpHeaders.acceptHeader: 'application/json'},
            body: jsonEncode({'email': email, 'password': password}),
          )
          .timeout(timeout);

      if (response.statusCode >= 200 && response.statusCode < 300) {
        final jsonMap = jsonDecode(response.body) as Map<String, dynamic>;
        final token = jsonMap['token'] as String?;
        if (token == null || token.isEmpty) {
          throw ApiException('Token missing in response');
        }
        return token;
      }

      throw ApiException(_extractError(response) ?? 'Login failed (${response.statusCode})');
    } on SocketException {
      String hint = '';
      if (Platform.isAndroid && (baseUrl.contains('localhost') || baseUrl.contains('127.0.0.1'))) {
        hint = ' On Android emulator, use http://10.0.2.2 instead of localhost.';
      } else if ((Platform.isMacOS || Platform.isIOS) && baseUrl.contains('localhost')) {
        hint = ' On macOS/iOS, try http://127.0.0.1 instead of localhost (some servers bind IPv4 only).';
      }
      throw ApiException('Network error while calling $uri. Please check your connection.$hint');
    } on HttpException catch (e) {
      throw ApiException('HTTP error: ${e.message}');
    } on FormatException {
      throw ApiException('Invalid server response.');
    }
  }

  Future<List<Map<String, dynamic>>> getItems({required String token}) async {
    return getItemsAt(token: token, endpoint: 'items');
  }

  Future<List<Map<String, dynamic>>> getItemsAt({required String token, required String endpoint}) async {
    final normalized = endpoint.startsWith('/') ? endpoint.substring(1) : endpoint;
    final uri = Uri.parse('$baseUrl/$normalized/verifications');
    try {
      final response = await _client
          .get(
            uri,
            headers: {HttpHeaders.acceptHeader: 'application/json', HttpHeaders.authorizationHeader: 'Bearer $token'},
          )
          .timeout(timeout);

      if (response.statusCode >= 200 && response.statusCode < 300) {
        final jsonVal = jsonDecode(response.body);
        if (jsonVal is List) {
          return jsonVal.cast<Map<String, dynamic>>();
        }
        if (jsonVal is Map) {
          final items = jsonVal['items'];
          if (items is List) {
            return items.cast<Map<String, dynamic>>();
          }
        }
        throw ApiException('Unexpected items format.');
      }

      throw ApiException(_extractError(response) ?? 'Failed to load items (${response.statusCode})');
    } on SocketException {
      String hint = '';
      if (Platform.isAndroid && (baseUrl.contains('localhost') || baseUrl.contains('127.0.0.1'))) {
        hint = ' On Android emulator, use http://10.0.2.2 instead of localhost.';
      } else if ((Platform.isMacOS || Platform.isIOS) && baseUrl.contains('localhost')) {
        hint = ' On macOS/iOS, try http://127.0.0.1 instead of localhost (some servers bind IPv4 only).';
      }
      throw ApiException('Network error while calling $uri. Please check your connection.$hint');
    } on HttpException catch (e) {
      throw ApiException('HTTP error: ${e.message}');
    } on FormatException {
      throw ApiException('Invalid server response.');
    }
  }

  Future<void> postVerificationDecision({
    required String token,
    required String endpoint,
    required dynamic id,
    required String code,
    bool isUsers = false,
    String? targetId,
  }) async {
    final normalized = endpoint.startsWith('/') ? endpoint.substring(1) : endpoint;
    final uri = Uri.parse('$baseUrl/$normalized/verifications/$id/decision');
    try {
      final Map<String, dynamic> body = {'code': code, 'accept': true};
      if (isUsers) {
        body['newPassword'] = '1';
        body['targetId'] = targetId;
      }
      final response = await _client
          .post(
            uri,
            headers: {
              HttpHeaders.contentTypeHeader: 'application/json',
              HttpHeaders.acceptHeader: 'application/json',
              HttpHeaders.authorizationHeader: 'Bearer $token',
            },
            body: jsonEncode(body),
          )
          .timeout(timeout);
      if (response.statusCode >= 200 && response.statusCode < 300) {
        return;
      }
      throw ApiException(_extractError(response) ?? 'Failed to submit decision (${response.statusCode})');
    } on SocketException {
      String hint = '';
      if (Platform.isAndroid && (baseUrl.contains('localhost') || baseUrl.contains('127.0.0.1'))) {
        hint = ' On Android emulator, use http://10.0.2.2 instead of localhost.';
      } else if ((Platform.isMacOS || Platform.isIOS) && baseUrl.contains('localhost')) {
        hint = ' On macOS/iOS, try http://127.0.0.1 instead of localhost (some servers bind IPv4 only).';
      }
      throw ApiException('Network error while calling $uri. Please check your connection.$hint');
    } on HttpException catch (e) {
      throw ApiException('HTTP error: ${e.message}');
    } on FormatException {
      throw ApiException('Invalid server response.');
    }
  }

  String? _extractError(http.Response r) {
    try {
      final m = jsonDecode(r.body);
      if (m is Map && m['error'] is String) return m['error'] as String;
      if (m is Map && m['message'] is String) return m['message'] as String;
    } catch (_) {}
    return null;
  }
}

class ApiException implements Exception {
  final String message;
  ApiException(this.message);
  @override
  String toString() => 'ApiException: $message';
}
