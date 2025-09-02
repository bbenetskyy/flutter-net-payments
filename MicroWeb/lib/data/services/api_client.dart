// Placeholder for your API client (e.g., using Dio)
import 'dart:convert';

import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

typedef TokenReader = String? Function();

class ApiClient {
  ApiClient({required String baseUrl, required TokenReader readAccessToken})
    : _readAccessToken = readAccessToken,
      _dio = Dio(
        BaseOptions(
          baseUrl: baseUrl,
          connectTimeout: const Duration(seconds: 10),
          receiveTimeout: const Duration(seconds: 20),
          headers: {'Content-Type': 'application/json'},
        ),
      ) {
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final t = await _readAccessToken();
          if (t != null && t.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $t';
          }
          handler.next(options);
        },
      ),
    );
    _dio.interceptors.add(
      LogInterceptor(
        request: true,
        requestHeader: true,
        requestBody: true,
        responseHeader: true,
        responseBody: true,
        error: true,
        logPrint: (obj) => debugPrint(obj.toString()),
      ),
    );
  }

  final Dio _dio;
  final TokenReader _readAccessToken;

  Future<dynamic> get(String path, {Map<String, dynamic>? query}) async {
    final res = await _dio.get(path, queryParameters: query);
    return res.data;
  }

  Future<dynamic> post(String path, {Object? body}) async {
    final res = await _dio.post(path, data: body);
    return res.data;
  }

  // Optional helper if you want manual calls
  Future<dynamic> put(String path, {Object? body}) async => (await _dio.put(path, data: body)).data;
  Future<dynamic> delete(String path) async => (await _dio.delete(path)).data;
}

// (Optional) decode JWT expiry if you want to refresh proactively
DateTime? jwtExpiry(String token) {
  try {
    final parts = token.split('.');
    if (parts.length != 3) return null;
    final payload = json.decode(utf8.decode(base64Url.decode(base64Url.normalize(parts[1]))));
    final exp = payload['exp'];
    if (exp is int) return DateTime.fromMillisecondsSinceEpoch(exp * 1000, isUtc: true);
  } catch (_) {}
  return null;
}
