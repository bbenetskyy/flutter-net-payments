import 'package:json_annotation/json_annotation.dart';
import 'package:uuid/uuid_value.dart';

class UuidValueConverter implements JsonConverter<UuidValue?, String?> {
  const UuidValueConverter();

  @override
  UuidValue? fromJson(String? json) {
    if (json == null || json.isEmpty) return null;
    try {
      return UuidValue.fromString(json);
    } catch (_) {
      return null;
    }
  }

  @override
  String? toJson(UuidValue? object) {
    return object?.uuid;
  }
}
