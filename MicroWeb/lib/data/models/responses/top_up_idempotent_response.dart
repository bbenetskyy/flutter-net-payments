import 'package:json_annotation/json_annotation.dart';

part 'top_up_idempotent_response.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class TopUpIdempotentResponse {
  TopUpIdempotentResponse({this.status, this.correlationId});

  @JsonKey(name: r'status', required: false, includeIfNull: false)
  final String? status;

  @JsonKey(name: r'correlationId', required: false, includeIfNull: false)
  final String? correlationId;

  factory TopUpIdempotentResponse.fromJson(Map<String, dynamic> json) => _$TopUpIdempotentResponseFromJson(json);
  Map<String, dynamic> toJson() => _$TopUpIdempotentResponseToJson(this);
}
