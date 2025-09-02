import 'package:web/web.dart' as web;

class TokenStorage {
  static const _kAccess = 'access_token';
  static const _kUserId = 'user_id';

  void saveTokens(String access) {
    final ls = web.window.localStorage;
    ls.setItem(_kAccess, access);
  }

  String? readAccess() {
    return web.window.localStorage.getItem(_kAccess);
  }

  void saveUserId(String userId) {
    final ls = web.window.localStorage;
    ls.setItem(_kUserId, userId);
  }

  String? readUserId() {
    return web.window.localStorage.getItem(_kUserId);
  }

  void clear() {
    final ls = web.window.localStorage;
    ls.removeItem(_kAccess);
    ls.removeItem(_kUserId);
  }
}
